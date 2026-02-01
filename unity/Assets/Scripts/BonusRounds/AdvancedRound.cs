using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// AdvancedRound: Same as Main round, but masks have visual effects
public class AdvancedRound : MonoBehaviour
{
    [SerializeField] private float roundDuration = 7f; // Slightly longer because harder
    [SerializeField] private int minMasks = 3;
    [SerializeField] private int maxMasks = 5;

    private List<int> currentActiveMasks = new List<int>();
    private HashSet<string> playersWhoTapped = new HashSet<string>();
    private HashSet<int> masksAlreadyAnimated = new HashSet<int>();
    private Dictionary<GameObject, MaskEffect> maskEffects = new Dictionary<GameObject, MaskEffect>();
    private float roundTimer;
    private bool roundActive = false;

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
        Debug.Log("🔍 StartAdvancedRound called!");

        currentActiveMasks.Clear();
        playersWhoTapped.Clear();
        masksAlreadyAnimated.Clear();
        maskEffects.Clear();
        roundActive = true;
        roundTimer = roundDuration;

        // SAME AS MAIN ROUND: Select 3-5 random masks from alive players
        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

        Debug.Log($"🔍 Found {alivePlayers.Count} alive players");

        if (alivePlayers.Count == 0)
        {
            Debug.LogError("❌ No alive players for Advanced Round!");
            return;
        }

        int maskCount = Random.Range(minMasks, maxMasks + 1);
        maskCount = Mathf.Min(maskCount, alivePlayers.Count);

        var selectedPlayers = alivePlayers.OrderBy(x => Random.value).Take(maskCount);
        currentActiveMasks = selectedPlayers.Select(p => p.MaskId).ToList();

        // Display masks on screen (same as Main round)
        if (MaskManager.Instance != null)
        {
            Debug.Log($"🔍 Calling DisplayMasks with {currentActiveMasks.Count} masks");
            MaskManager.Instance.DisplayMasks(currentActiveMasks);
            Debug.Log("🔍 DisplayMasks completed");

            // NEW: Apply random visual effects to make it harder!
            ApplyRandomEffects();
        }
        else
        {
            Debug.LogError("❌ MaskManager.Instance is NULL!");
        }

        Debug.Log($"💀 Advanced Round! Masks with effects: {string.Join(", ", currentActiveMasks.Select(m => $"#{m}"))}");
        
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

            Debug.Log($"  Applied {effect} effect to mask at {rt.anchoredPosition}");
        }

        Debug.Log($"✅ Applied effects to {maskEffects.Count} masks");
    }

    void Update()
    {
        if (!roundActive) return;

        // Countdown timer
        roundTimer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRoundTimer(roundTimer);
        }

        // Apply visual effects to masks
        UpdateMaskEffects();

        if (roundTimer <= 0)
        {
            EvaluateRound();
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
        if (player != null && currentActiveMasks.Contains(player.MaskId))
        {
            if (!masksAlreadyAnimated.Contains(player.MaskId))
            {
                masksAlreadyAnimated.Add(player.MaskId);
                if (MaskManager.Instance != null)
                {
                    MaskManager.Instance.AnimateSafeTap(player.MaskId);
                }
            }
        }
    }

    void EvaluateRound()
    {
        roundActive = false;

        Debug.Log("📊 Evaluating Advanced Round...");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
        }

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.ClearMasks();
        }

        maskEffects.Clear();

        // SAME AS MAIN ROUND: Evaluate eliminations
        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        int eliminatedThisRound = 0;

        foreach (var player in alivePlayers)
        {
            bool playerTapped = playersWhoTapped.Contains(player.Id);
            bool playerMaskWasActive = currentActiveMasks.Contains(player.MaskId);

            // Eliminate if:
            // 1. Tapped when mask was NOT active (wrong tap)
            // 2. Didn't tap when mask WAS active (missed tap)
            if (playerTapped && !playerMaskWasActive)
            {
                PlayerManager.Instance.EliminatePlayer(player.Id, "wrong_tap");
                eliminatedThisRound++;
            }
            else if (!playerTapped && playerMaskWasActive)
            {
                PlayerManager.Instance.EliminatePlayer(player.Id, "missed_tap");
                eliminatedThisRound++;
            }
        }

        Debug.Log($"📊 Advanced Round complete. {eliminatedThisRound} players eliminated.");

        // Notify GameManager to continue
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
    }
}
