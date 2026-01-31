using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PrecisionRound : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float targetTime = 5.0f; // Target 5 seconds
    [SerializeField] private float roundDuration = 10f;

    [Header("Elimination Rules")]
    [SerializeField] private int minEliminations = 1; // Always eliminate at least this many
    [SerializeField] private int maxEliminations = 999; // Never eliminate more than this
    [SerializeField] private bool eliminateWorstPercent = true; // If true, eliminate worst X%
    [SerializeField] [Range(0f, 1f)] private float eliminationPercent = 0.5f; // 50% = half eliminated

    [Header("Visual Feedback")]
    [SerializeField] private Transform maskDisplayContainer; // Container for showing tapped masks
    [SerializeField] private GameObject maskDisplayPrefab; // Prefab with Image component
    [SerializeField] private float maskScale = 1.0f; // Uniform size for commitment display

    private Dictionary<string, long> playerTapTimes = new Dictionary<string, long>();
    private List<GameObject> displayedMasks = new List<GameObject>();
    private long roundStartTime;
    private bool active = false;
    private float timer;

    void Start()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnTapReceived += OnTapReceived;
        }
    }

    public void StartPrecisionRound()
    {
        active = true;
        timer = roundDuration;
        playerTapTimes.Clear();
        roundStartTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

        // Clear any previous masks
        ClearDisplayedMasks();

        Debug.Log($"🎯 Precision Round: Tap at exactly {targetTime:F1} seconds!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundTitle($"PRECISION ROUND\n\nTap at exactly {targetTime:F1}s!");
        }
    }

    void Update()
    {
        if (!active) return;

        timer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRoundTimer(timer);
        }

        if (timer <= 0)
        {
            EvaluatePrecisionRound();
        }
    }

    void OnTapReceived(TapData tapData)
    {
        if (!active) return;

        // Record first tap only
        if (!playerTapTimes.ContainsKey(tapData.PlayerId))
        {
            playerTapTimes[tapData.PlayerId] = tapData.Timestamp;

            float elapsed = (tapData.Timestamp - roundStartTime) / 1000f;
            float deviation = Mathf.Abs(elapsed - targetTime);
            Debug.Log($"Player {tapData.PlayerId} tapped at {elapsed:F2}s (deviation: {deviation:F2}s)");

            // Display mask on screen to show commitment
            DisplayTappedMask(tapData.PlayerId);
        }
    }

    void EvaluatePrecisionRound()
    {
        active = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
        }

        // Calculate deviation from target time
        var deviations = new Dictionary<string, float>();

        foreach (var kvp in playerTapTimes)
        {
            float elapsed = (kvp.Value - roundStartTime) / 1000f;
            float deviation = Mathf.Abs(elapsed - targetTime);
            deviations[kvp.Key] = deviation;
        }

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

        // Calculate how many to eliminate
        int eliminateCount;
        if (eliminateWorstPercent)
        {
            eliminateCount = Mathf.CeilToInt(alivePlayers.Count * eliminationPercent);
        }
        else
        {
            eliminateCount = minEliminations;
        }

        // Enforce min/max bounds & ensure at least 1 is eliminated
        eliminateCount = Mathf.Clamp(eliminateCount, minEliminations, Mathf.Min(maxEliminations, alivePlayers.Count));
        eliminateCount = Mathf.Max(eliminateCount, 1); // ALWAYS eliminate at least 1

        Debug.Log($"🎯 Evaluating Precision Round: {alivePlayers.Count} alive, eliminating {eliminateCount}");

        // Sort by deviation (worst first for elimination)
        var sorted = deviations.OrderByDescending(kvp => kvp.Value).ToList();

        int eliminatedCount = 0;

        // First: Eliminate anyone who didn't tap
        foreach (var player in alivePlayers.ToList())
        {
            if (!deviations.ContainsKey(player.Id))
            {
                PlayerManager.Instance.EliminatePlayer(player.Id, "failed_bonus");
                eliminatedCount++;
            }
        }

        // Second: Eliminate worst timing players until we hit eliminateCount
        if (eliminatedCount < eliminateCount)
        {
            foreach (var kvp in sorted)
            {
                if (eliminatedCount >= eliminateCount) break;

                var player = PlayerManager.Instance.GetPlayer(kvp.Key);
                if (player != null && player.IsAlive)
                {
                    Debug.Log($"  Eliminating {player.Id}: deviation {kvp.Value:F2}s from target {targetTime:F1}s");
                    PlayerManager.Instance.EliminatePlayer(player.Id, "failed_bonus");
                    eliminatedCount++;
                }
            }
        }

        int remaining = PlayerManager.Instance.GetAliveCount();
        Debug.Log($"🎯 Precision Round complete. {eliminatedCount} eliminated, {remaining} survived.");

        // Clear displayed masks
        ClearDisplayedMasks();

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
    }

    void DisplayTappedMask(string playerId)
    {
        if (maskDisplayContainer == null || maskDisplayPrefab == null)
        {
            Debug.LogWarning("⚠️ Mask display container or prefab not assigned!");
            return;
        }

        var player = PlayerManager.Instance.GetPlayer(playerId);
        if (player == null)
        {
            Debug.LogWarning($"⚠️ Player {playerId} not found!");
            return;
        }

        // Create mask display
        GameObject maskObj = Instantiate(maskDisplayPrefab, maskDisplayContainer);
        UnityEngine.UI.Image img = maskObj.GetComponent<UnityEngine.UI.Image>();

        Debug.Log($"🔍 DisplayTappedMask: Image component = {(img != null ? "FOUND" : "NULL")}");
        Debug.Log($"🔍 MaskManager.Instance = {(MaskManager.Instance != null ? "FOUND" : "NULL")}");

        if (img != null && MaskManager.Instance != null)
        {
            Sprite maskSprite = MaskManager.Instance.GetMaskSprite(player.MaskId);
            Debug.Log($"🔍 GetMaskSprite({player.MaskId}) = {(maskSprite != null ? "FOUND" : "NULL")}");

            if (maskSprite != null)
            {
                img.sprite = maskSprite;
                Debug.Log($"✅ Set sprite for Mask #{player.MaskId}");
            }
            else
            {
                Debug.LogWarning($"⚠️ No sprite found for Mask #{player.MaskId}!");
            }
        }

        // Uniform scale for commitment display
        RectTransform rt = maskObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one * maskScale;
        }

        displayedMasks.Add(maskObj);

        Debug.Log($"  Displayed Mask #{player.MaskId} (commitment #{displayedMasks.Count})");

        // Arrange masks in grid
        ArrangeMasks();
    }

    void ArrangeMasks()
    {
        if (displayedMasks.Count == 0) return;

        // Simple horizontal flow layout
        int masksPerRow = 10;
        float spacing = 150f;
        float rowSpacing = 150f;

        for (int i = 0; i < displayedMasks.Count; i++)
        {
            int row = i / masksPerRow;
            int col = i % masksPerRow;

            RectTransform rt = displayedMasks[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                float xPos = (col - masksPerRow / 2f) * spacing;
                float yPos = -row * rowSpacing;
                rt.anchoredPosition = new Vector2(xPos, yPos);
            }
        }
    }

    void ClearDisplayedMasks()
    {
        foreach (var maskObj in displayedMasks)
        {
            if (maskObj != null)
            {
                Destroy(maskObj);
            }
        }
        displayedMasks.Clear();
    }
}
