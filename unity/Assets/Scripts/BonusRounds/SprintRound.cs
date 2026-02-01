using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SprintRound : MonoBehaviour
{
    [SerializeField] private int requiredTaps = 100;
    [SerializeField] private float roundDuration = 30f;
    [SerializeField] private Transform sprintContainer; // Container for racing masks
    [SerializeField] private GameObject sprintMaskPrefab; // Small mask prefab for racing

    private Dictionary<string, int> playerTapCounts = new Dictionary<string, int>();
    private Dictionary<int, GameObject> maskIdToRacerObject = new Dictionary<int, GameObject>(); // maskId -> racer GameObject
    private Dictionary<int, float> maskIdToTargetX = new Dictionary<int, float>(); // For smooth tweening
    private List<GameObject> activeRacers = new List<GameObject>();
    private float timer;
    private bool active = false;

    private const float START_X = -740f; // 220px from left edge (1920/2 - 220)
    private const float END_X = 740f;    // 220px from right edge
    private const float MASK_SIZE = 50f;  // Small size for racing
    private const float TWEEN_SPEED = 10f; // Smooth movement speed

    void Start()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnTapReceived += OnTapReceived;
        }
    }

    public void StartSprintRound()
    {
        active = true;
        timer = roundDuration;
        playerTapCounts.Clear();
        maskIdToRacerObject.Clear();
        maskIdToTargetX.Clear();

        // Reset tap counts in PlayerManager
        PlayerManager.Instance.ResetTapCounts();

        Debug.Log($"🏃 Sprint Round: Tap {requiredTaps} times in {roundDuration} seconds!");

        // Display all alive players' masks at the bottom
        DisplayRacingMasks();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundInfo($"SPRINT ROUND", $"Tap {requiredTaps}x in {roundDuration}s!");
        }
    }

    void DisplayRacingMasks()
    {
        // Clear any existing racers
        foreach (var racer in activeRacers)
        {
            Destroy(racer);
        }
        activeRacers.Clear();

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        int playerCount = alivePlayers.Count;

        Debug.Log($"🏁 DisplayRacingMasks: {playerCount} alive players");
        Debug.Log($"🏁 Sprint Container: {(sprintContainer != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"🏁 Sprint Mask Prefab: {(sprintMaskPrefab != null ? "ASSIGNED" : "NULL")}");

        if (playerCount == 0)
        {
            Debug.LogError("❌ No alive players for Sprint Round!");
            return;
        }

        if (sprintContainer == null)
        {
            Debug.LogError("❌ Sprint Container not assigned! Link it in Inspector.");
            return;
        }

        if (sprintMaskPrefab == null)
        {
            Debug.LogError("❌ Sprint Mask Prefab not assigned! Link it in Inspector.");
            return;
        }

        // Calculate layout (arrange in vertical lanes)
        // Use 2 columns if more than 20 players
        bool useDoubleColumn = playerCount > 20;
        int masksPerColumn = useDoubleColumn ? Mathf.CeilToInt(playerCount / 2f) : playerCount;

        float verticalSpacing = 1080f / (masksPerColumn + 1); // Screen height divided by masks
        float columnOffset = useDoubleColumn ? 100f : 0f; // Horizontal offset for second column

        int currentColumn = 0;
        int currentLane = 0;

        foreach (var player in alivePlayers)
        {
            // Create racer object
            GameObject racerObj = Instantiate(sprintMaskPrefab, sprintContainer);
            RectTransform rt = racerObj.GetComponent<RectTransform>();
            Image img = racerObj.GetComponent<Image>();

            if (rt != null)
            {
                // Position at left side of screen, spread vertically
                float xPos = START_X + (currentColumn * columnOffset);
                float yPos = (currentLane + 1) * verticalSpacing - (1080f / 2f);

                rt.anchoredPosition = new Vector2(xPos, yPos);
                rt.sizeDelta = new Vector2(MASK_SIZE, MASK_SIZE);

                // Initialize target position
                maskIdToTargetX[player.MaskId] = xPos;

                Debug.Log($"  Mask #{player.MaskId} positioned at ({xPos:F0}, {yPos:F0})");
            }
            else
            {
                Debug.LogError($"  Mask #{player.MaskId} has no RectTransform!");
            }

            if (img != null && MaskManager.Instance != null)
            {
                Sprite maskSprite = MaskManager.Instance.GetMaskSprite(player.MaskId);
                if (maskSprite != null)
                {
                    img.sprite = maskSprite;
                    Debug.Log($"  Mask #{player.MaskId} sprite loaded");
                }
                else
                {
                    Debug.LogWarning($"  Mask #{player.MaskId} sprite is NULL");
                    // Fallback: Set a visible color so we can see it
                    img.color = Color.HSVToRGB((player.MaskId * 6f) / 360f, 0.7f, 0.9f);
                }
            }
            else
            {
                Debug.LogError($"  Mask #{player.MaskId} Image component missing!");
            }

            maskIdToRacerObject[player.MaskId] = racerObj;
            activeRacers.Add(racerObj);

            // Move to next lane
            currentLane++;
            if (useDoubleColumn && currentLane >= masksPerColumn)
            {
                currentLane = 0;
                currentColumn = 1;
            }
        }

        Debug.Log($"🏁 Created {activeRacers.Count} racing masks");
    }

    void Update()
    {
        if (!active) return;

        timer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRoundTimer(timer);
        }

        // Smooth tween all racers towards their target positions
        foreach (var kvp in maskIdToRacerObject)
        {
            int maskId = kvp.Key;
            GameObject racerObj = kvp.Value;

            if (racerObj != null && maskIdToTargetX.ContainsKey(maskId))
            {
                RectTransform rt = racerObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    float targetX = maskIdToTargetX[maskId];
                    float currentX = rt.anchoredPosition.x;
                    float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * TWEEN_SPEED);
                    rt.anchoredPosition = new Vector2(newX, rt.anchoredPosition.y);
                }
            }
        }

        if (timer <= 0)
        {
            EvaluateSprintRound();
        }
    }

    void OnTapReceived(TapData tapData)
    {
        if (!active) return;

        // SIMPLIFIED: Each tap event from mobile = 5 actual taps
        // Mobile throttles to send every 5th tap, so we multiply by 5 here
        if (!playerTapCounts.ContainsKey(tapData.PlayerId))
        {
            playerTapCounts[tapData.PlayerId] = 0;
        }
        playerTapCounts[tapData.PlayerId] += 5;

        Debug.Log($"Sprint tap from {tapData.PlayerId}: now at {playerTapCounts[tapData.PlayerId]} taps");

        // Update player's tap count in PlayerManager
        var player = PlayerManager.Instance.GetPlayer(tapData.PlayerId);
        if (player != null)
        {
            player.TapCount = playerTapCounts[tapData.PlayerId];

            // Update racer position
            UpdateRacerPosition(player.MaskId, playerTapCounts[tapData.PlayerId]);
        }
    }

    void UpdateRacerPosition(int maskId, int tapCount)
    {
        if (!maskIdToRacerObject.ContainsKey(maskId)) return;

        // Calculate progress (0 to 1)
        float progress = Mathf.Clamp01((float)tapCount / requiredTaps);

        // Calculate target X position (left to right)
        float targetX = Mathf.Lerp(START_X, END_X, progress);

        // Store target position for smooth tweening in Update()
        maskIdToTargetX[maskId] = targetX;
    }

    void EvaluateSprintRound()
    {
        active = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
        }

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        int eliminatedCount = 0;

        foreach (var player in alivePlayers)
        {
            int taps = playerTapCounts.ContainsKey(player.Id) ? playerTapCounts[player.Id] : 0;

            Debug.Log($"Player {player.Id} (Mask #{player.MaskId}): {taps} taps");

            if (taps < requiredTaps)
            {
                PlayerManager.Instance.EliminatePlayer(player.Id, "failed_bonus");
                eliminatedCount++;
            }
        }

        Debug.Log($"Sprint Round complete. {eliminatedCount} players eliminated.");

        // Clear racing masks
        foreach (var racer in activeRacers)
        {
            Destroy(racer);
        }
        activeRacers.Clear();
        maskIdToRacerObject.Clear();
        maskIdToTargetX.Clear();

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
    }
}
