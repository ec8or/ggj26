using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// AdvancedRound: Same as Snap round, but masks have random visual effects
public class AdvancedRound : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private float roundDuration = 5f; // Seconds to tap
    [SerializeField] private int minMasks = 3;
    [SerializeField] private int maxMasks = 5;
    [SerializeField] private int snapsPerRound = 5; // How many snaps before moving to next round
    [SerializeField] private int defaultRoundEliminationCount = 1;
    [SerializeField] private float endSnapDelay = 2f;

    private List<int> currentActiveMasks = new List<int>();
    private HashSet<string> playersWhoTapped = new HashSet<string>();
    private HashSet<int> masksAlreadyAnimated = new HashSet<int>();
    private Dictionary<GameObject, MaskEffect> maskEffects = new Dictionary<GameObject, MaskEffect>();
    private float roundTimer;
    private bool roundActive = false;
    private int currentSnap = 0;
    private int totalSnaps = 0;
    private int currentRoundEliminationCount = 1;

    // Visual effect types
    private enum EffectType { Spinning, Pulsing, Blinking, MovingHorizontal, MovingVertical }

    private class MaskEffect
    {
        public EffectType type;
        public float speed;
        public float amplitude;
        public Vector2 originalPosition;
        public Vector3 originalScale;
    }

    void Start()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnTapReceived += OnTapReceived;
        }
    }

    public void StartAdvancedRound()
    {
        currentSnap = 0;
        currentRoundEliminationCount = defaultRoundEliminationCount;
        totalSnaps = snapsPerRound;

        // Show title screen only once at the start
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundInfo($"ADVANCED SNAP", $"{totalSnaps} Snaps with Effects!", 2f);
        }

        Invoke(nameof(StartNextSnap), 2f); // Delay before first snap
    }

    private void StartNextSnap()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.ClearMasks();
        }

        maskEffects.Clear();
        currentSnap++;

        if (currentSnap > totalSnaps || PlayerManager.Instance.GetAlivePlayers().Count < 3)
        {
            // All snaps complete, move to next round
            CompleteRound();
            return;
        }

        currentActiveMasks.Clear();
        playersWhoTapped.Clear();
        masksAlreadyAnimated.Clear();
        roundActive = true;
        roundTimer = roundDuration;

        // Select 3-5 random masks from alive players
        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

        if (alivePlayers.Count == 0)
        {
            Debug.LogError("No alive players to start Advanced round!");
            CompleteRound();
            return;
        }

        int maskCount = Random.Range(minMasks, maxMasks + 1);
        maskCount = Mathf.Min(maskCount, alivePlayers.Count);

        var selectedPlayers = alivePlayers.OrderBy(x => Random.value).Take(maskCount);
        currentActiveMasks = selectedPlayers.Select(p => p.MaskId).ToList();

        // Display masks on screen
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.DisplayMasks(currentActiveMasks);

            // NEW: Apply random visual effects to make it harder!
            ApplyRandomEffects();
        }

        // Update UI with snap counter
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVisualTimer();
        }

        Debug.Log($"🎭 Advanced Snap {currentSnap}/{totalSnaps} active! Masks shown: {string.Join(", ", currentActiveMasks.Select(m => $"#{m}"))}");
    }

    void ApplyRandomEffects()
    {
        // Get all active mask GameObjects from MaskManager
        var maskDisplays = MaskManager.Instance.GetActiveMaskDisplays();

        foreach (GameObject maskObj in maskDisplays)
        {
            RectTransform rt = maskObj.GetComponent<RectTransform>();
            if (rt == null) continue;

            // Randomly pick an effect for this mask
            EffectType effect = (EffectType)Random.Range(0, System.Enum.GetValues(typeof(EffectType)).Length);

            MaskEffect maskEffect = new MaskEffect
            {
                type = effect,
                speed = Random.Range(1.5f, 3f),
                amplitude = Random.Range(50f, 120f),
                originalPosition = rt.anchoredPosition,
                originalScale = rt.localScale
            };

            maskEffects[maskObj] = maskEffect;

            Debug.Log($"  Applied {effect} effect to mask");
        }

        Debug.Log($"✅ Applied effects to {maskEffects.Count} masks");
    }

    void Update()
    {
        if (!roundActive) return;

        roundTimer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRoundTimer(roundTimer);
            UIManager.Instance.UpdateVisualTimer(roundTimer, roundDuration);
        }

        // Apply visual effects to masks
        UpdateMaskEffects();

        if (roundTimer <= 0)
        {
            EndSnapRound();
        }
    }

    void UpdateMaskEffects()
    {
        foreach (var kvp in maskEffects)
        {
            GameObject maskObj = kvp.Key;
            if (maskObj == null) continue;

            MaskEffect effect = kvp.Value;
            RectTransform rt = maskObj.GetComponent<RectTransform>();
            if (rt == null) continue;

            switch (effect.type)
            {
                case EffectType.Spinning:
                    // Rotate continuously
                    rt.Rotate(0, 0, effect.speed * 100f * Time.deltaTime);
                    break;

                case EffectType.Pulsing:
                    // Scale up and down
                    float pulseScale = 1f + Mathf.Sin(Time.time * effect.speed) * 0.3f;
                    rt.localScale = effect.originalScale * pulseScale;
                    break;

                case EffectType.Blinking:
                    // Fade in and out
                    var image = maskObj.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                    {
                        float alpha = 0.3f + (Mathf.Sin(Time.time * effect.speed * 2f) + 1f) * 0.35f; // Range: 0.3 to 1.0
                        Color color = image.color;
                        color.a = alpha;
                        image.color = color;
                    }
                    break;

                case EffectType.MovingHorizontal:
                    // Move left and right
                    float xOffset = Mathf.Sin(Time.time * effect.speed) * effect.amplitude;
                    rt.anchoredPosition = new Vector2(effect.originalPosition.x + xOffset, effect.originalPosition.y);
                    break;

                case EffectType.MovingVertical:
                    // Move up and down
                    float yOffset = Mathf.Sin(Time.time * effect.speed) * effect.amplitude;
                    rt.anchoredPosition = new Vector2(effect.originalPosition.x, effect.originalPosition.y + yOffset);
                    break;
            }
        }
    }

    void OnTapReceived(TapData tapData)
    {
        if (!roundActive) return;

        playersWhoTapped.Add(tapData.PlayerId);
        Debug.Log($"✓ Advanced tap from {tapData.PlayerId}");

        // Animate mask if it's a correct tap (only once per round)
        var player = PlayerManager.Instance.GetPlayer(tapData.PlayerId);
        if (player != null)
        {
            if (currentActiveMasks.Contains(player.MaskId))
            {
                // Correct tap! Animate the mask (only if not already animated)
                if (!masksAlreadyAnimated.Contains(player.MaskId))
                {
                    masksAlreadyAnimated.Add(player.MaskId);
                    if (MaskManager.Instance != null)
                    {
                        MaskManager.Instance.AnimateSafeTap(player.MaskId);
                        MaskManager.Instance.ShowTickOverlay(player.MaskId, 20f);
                    }
                }
            }
            else
            {
                // Invalid tap - eliminate immediately
                PlayerManager.Instance.EliminatePlayer(player.Id, "wrong_tap");
            }
        }

        CheckEndSnapRound();
    }

    void CheckEndSnapRound()
    {
        Debug.Log($"CheckEndSnapRound - playersWhoTapped.Count: {playersWhoTapped.Count}, currentActiveMasks.Count: {currentActiveMasks.Count}, currentRoundEliminationCount: {currentRoundEliminationCount}");

        var activePlayersLeft = currentActiveMasks.Count;
        foreach (var currentActiveMask in currentActiveMasks)
        {
            var player = PlayerManager.Instance.GetPlayerByMaskId(currentActiveMask);
            if (playersWhoTapped.Contains(player.Id))
            {
                activePlayersLeft--;
            }
        }
        if (activePlayersLeft <= currentRoundEliminationCount)
        {
            EndSnapRound();
        }
    }

    void EndSnapRound()
    {
        roundActive = false;

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        int eliminatedThisSnap = 0;

        // First pass: Show overlays and eliminate
        foreach (var player in alivePlayers)
        {
            bool playerTapped = playersWhoTapped.Contains(player.Id);
            bool playerMaskWasActive = currentActiveMasks.Contains(player.MaskId);

            // Show overlay feedback
            if (playerMaskWasActive)
            {
                // Their mask was shown
                if (playerTapped)
                {
                    // Correct! Show tick
                    MaskManager.Instance.ShowTickOverlay(player.MaskId, 2.0f);
                }
                else
                {
                    // Didn't tap - show cross and eliminate
                    MaskManager.Instance.ShowCrossOverlay(player.MaskId, 2.0f);
                    PlayerManager.Instance.EliminatePlayer(player.Id, "missed_tap");
                    eliminatedThisSnap++;
                }
            }
            else
            {
                // Their mask was NOT shown
                if (playerTapped)
                {
                    // Wrong tap! Already eliminated immediately
                    eliminatedThisSnap++;
                }
                // If they didn't tap and their mask wasn't shown = correct (do nothing)
            }
        }

        Debug.Log($"📊 Advanced Snap {currentSnap}/{totalSnaps} complete. {eliminatedThisSnap} players eliminated.");

        // Start next snap after delay
        Invoke(nameof(StartNextSnap), endSnapDelay);
    }

    private void CompleteRound()
    {
        Debug.Log($"📊 Advanced Round complete! All {totalSnaps} snaps finished.");

        maskEffects.Clear();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
            UIManager.Instance.HideVisualTimer();
        }

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
    }
}
