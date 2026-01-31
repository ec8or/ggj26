using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReactionRound : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private float reactionWindow = 3f;

    [Header("Elimination Rules")]
    [SerializeField] private int minEliminations = 1; // Always eliminate at least this many
    [SerializeField] private int maxEliminations = 999; // Never eliminate more than this
    [SerializeField] private bool eliminateSlowestPercent = true; // If true, eliminate slowest X%
    [SerializeField] [Range(0f, 1f)] private float eliminationPercent = 0.5f; // 50% = half eliminated

    [Header("Visual Feedback")]
    [SerializeField] private Transform maskDisplayContainer; // Container for showing tapped masks
    [SerializeField] private GameObject maskDisplayPrefab; // Prefab with Image component
    [SerializeField] private float firstPlaceScale = 2.5f; // Biggest mask
    [SerializeField] private float lastPlaceScale = 0.5f; // Smallest mask

    private Dictionary<string, long> playerReactionTimes = new Dictionary<string, long>();
    private List<GameObject> displayedMasks = new List<GameObject>();
    private int tapOrder = 0; // Track order of taps
    private long goSignalTime;
    private bool waitingForGo = false;
    private bool active = false;
    private float timer;

    void Start()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnTapReceived += OnTapReceived;
        }
    }

    public void StartReactionRound()
    {
        waitingForGo = true;
        active = false;
        playerReactionTimes.Clear();
        tapOrder = 0;

        // Clear any previous masks
        ClearDisplayedMasks();

        Debug.Log("⚡ Reaction Round: Wait for GO!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundTitle("REACTION ROUND\n\nWAIT...");
            UIManager.Instance.ShowRedIndicator(); // Show RED square
        }

        // Wait 2-5 seconds before GO signal
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        Invoke(nameof(ShowGoSignal), waitTime);
    }

    void ShowGoSignal()
    {
        waitingForGo = false;
        active = true;
        goSignalTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        timer = reactionWindow;

        Debug.Log("⚡ GO!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundTitle("REACTION ROUND\n\nGO!!!");
            UIManager.Instance.ShowGreenIndicator(); // Show GREEN square
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
            EvaluateReactionRound();
        }
    }

    void OnTapReceived(TapData tapData)
    {
        if (waitingForGo)
        {
            // Tapped too early - eliminate immediately
            Debug.Log($"Player {tapData.PlayerId} tapped too early!");
            PlayerManager.Instance.EliminatePlayer(tapData.PlayerId, "too_early");
            return;
        }

        if (!active) return;

        // Record reaction time (only first tap counts)
        if (!playerReactionTimes.ContainsKey(tapData.PlayerId))
        {
            long reactionTime = tapData.Timestamp - goSignalTime;
            playerReactionTimes[tapData.PlayerId] = reactionTime;
            tapOrder++;

            Debug.Log($"Player {tapData.PlayerId} reaction time: {reactionTime}ms (place #{tapOrder})");

            // Display mask on screen with size based on order (first = biggest)
            DisplayTappedMask(tapData.PlayerId, tapOrder);
        }
    }

    void EvaluateReactionRound()
    {
        active = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
            UIManager.Instance.HideReactionIndicators(); // Hide red/green squares
        }

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

        // Calculate how many to eliminate
        int eliminateCount;
        if (eliminateSlowestPercent)
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

        Debug.Log($"⚡ Evaluating Reaction Round: {alivePlayers.Count} alive, eliminating {eliminateCount}");

        // Sort by reaction time (slowest first for elimination)
        var sorted = playerReactionTimes.OrderByDescending(kvp => kvp.Value).ToList();

        int eliminatedCount = 0;

        // First: Eliminate anyone who didn't tap
        foreach (var player in alivePlayers.ToList())
        {
            if (!playerReactionTimes.ContainsKey(player.Id))
            {
                PlayerManager.Instance.EliminatePlayer(player.Id, "too_slow");
                eliminatedCount++;
            }
        }

        // Second: Eliminate slowest players until we hit eliminateCount
        if (eliminatedCount < eliminateCount)
        {
            foreach (var kvp in sorted)
            {
                if (eliminatedCount >= eliminateCount) break;

                var player = PlayerManager.Instance.GetPlayer(kvp.Key);
                if (player != null && player.IsAlive)
                {
                    PlayerManager.Instance.EliminatePlayer(player.Id, "too_slow");
                    eliminatedCount++;
                }
            }
        }

        int remaining = PlayerManager.Instance.GetAliveCount();
        Debug.Log($"⚡ Reaction Round complete. {eliminatedCount} eliminated, {remaining} survived.");

        // Clear displayed masks
        ClearDisplayedMasks();

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
    }

    void DisplayTappedMask(string playerId, int order)
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

        // Scale based on order: first = biggest, last = smallest
        int totalPlayers = PlayerManager.Instance.GetAliveCount();
        float t = (order - 1) / Mathf.Max(1f, totalPlayers - 1); // 0 to 1
        float scale = Mathf.Lerp(firstPlaceScale, lastPlaceScale, t);

        RectTransform rt = maskObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one * scale;
        }

        displayedMasks.Add(maskObj);

        Debug.Log($"  Displayed Mask #{player.MaskId} at scale {scale:F2}x (order {order})");

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
