using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReactionRound : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float minRedLightTime = 2f;
    [SerializeField] private float maxRedLightTime = 5f;
    [SerializeField] private float greenLightDuration = 2f; // How long to tap after GO

    private Dictionary<string, long> playerReactionTimes = new Dictionary<string, long>();
    private HashSet<string> tappedDuringRed = new HashSet<string>(); // Track red light tappers (but don't eliminate)
    private HashSet<string> markedForElimination = new HashSet<string>(); // Track who gets eliminated
    private long goSignalTime;
    private bool waitingForSpace = false; // Waiting for space press to start
    private bool waitingForGo = false; // Red light active
    private bool greenLightActive = false;
    private float timer;
    private int cycleCount = 0;

    void Start()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnTapReceived += OnTapReceived;
        }
    }

    public void StartReactionRound()
    {
        cycleCount = 0;
        waitingForSpace = true;
        tappedDuringRed.Clear();

        Debug.Log("⚡ REACTION ROUND: Red Light showing - Press SPACE to start!");

        // Display ALL remaining masks on screen
        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        var allMaskIds = alivePlayers.Select(p => p.MaskId).ToList();

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.DisplayMasks(allMaskIds);
        }

        Debug.Log($"⚡ Showing all {allMaskIds.Count} remaining masks");

        // Show red light immediately
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRedIndicator();
        }

        // Wait for space press (handled in Update)
    }

    void StartRedLight()
    {
        waitingForGo = true;
        greenLightActive = false;
        playerReactionTimes.Clear();
        markedForElimination.Clear();

        Debug.Log("🔴 RED LIGHT - Don't tap!");

        // Red light is already showing, just wait for random time before GO
        float waitTime = Random.Range(minRedLightTime, maxRedLightTime);
        Invoke(nameof(StartGreenLight), waitTime);
    }

    void StartGreenLight()
    {
        waitingForGo = false;
        greenLightActive = true;
        goSignalTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        timer = greenLightDuration;

        Debug.Log("🟢 GREEN LIGHT - TAP NOW!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGreenIndicator();
            UIManager.Instance.ShowVisualTimer();
        }

        // Crosses are already showing from red light phase (with 999f duration)
    }

    void Update()
    {
        // Wait for space press to start
        if (waitingForSpace && Input.GetKeyDown(KeyCode.Space))
        {
            waitingForSpace = false;
            StartRedLight();
            return;
        }

        if (!greenLightActive) return;

        timer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRoundTimer(timer);
            UIManager.Instance.UpdateVisualTimer(timer, greenLightDuration);
        }

        if (timer <= 0)
        {
            EvaluateRound();
        }
    }

    void OnTapReceived(TapData tapData)
    {
        var player = PlayerManager.Instance.GetPlayer(tapData.PlayerId);
        if (player == null || !player.IsAlive) return;

        if (waitingForSpace) return; // Ignore taps before space is pressed

        if (waitingForGo)
        {
            // Tapped during RED - mark but don't eliminate!
            if (!tappedDuringRed.Contains(tapData.PlayerId))
            {
                Debug.Log($"❌ Player {tapData.PlayerId} (Mask #{player.MaskId}) tapped during RED - marked!");
                tappedDuringRed.Add(tapData.PlayerId);

                // Show cross permanently (until round ends)
                if (MaskManager.Instance != null)
                {
                    MaskManager.Instance.ShowCrossOverlay(player.MaskId, 999f);
                }
            }
            return;
        }

        if (!greenLightActive) return;

        // Record reaction time (only first tap counts)
        if (!playerReactionTimes.ContainsKey(tapData.PlayerId))
        {
            long reactionTime = tapData.Timestamp - goSignalTime;
            playerReactionTimes[tapData.PlayerId] = reactionTime;

            Debug.Log($"✓ Player {tapData.PlayerId} (Mask #{player.MaskId}) reaction: {reactionTime}ms");

            // Show tick immediately!
            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.ShowTickOverlay(player.MaskId, 999f);
            }

            // Check if this player was the second-to-last to tap
            CheckForLastPlayer();
        }
    }

    void CheckForLastPlayer()
    {
        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

        // Count how many players are eligible (didn't tap during red)
        int eligibleCount = 0;
        foreach (var player in alivePlayers)
        {
            if (!tappedDuringRed.Contains(player.Id))
            {
                eligibleCount++;
            }
        }

        // If all but one have tapped, mark the last one
        if (playerReactionTimes.Count == eligibleCount - 1)
        {
            // Find the player who hasn't tapped yet
            foreach (var player in alivePlayers)
            {
                if (!tappedDuringRed.Contains(player.Id) && !playerReactionTimes.ContainsKey(player.Id))
                {
                    Debug.Log($"❌ Mask #{player.MaskId} is the LAST one - showing cross immediately!");

                    if (MaskManager.Instance != null)
                    {
                        MaskManager.Instance.ShowCrossOverlay(player.MaskId, 999f);
                    }

                    markedForElimination.Add(player.Id);
                    break;
                }
            }
        }
    }

    void EvaluateRound()
    {
        greenLightActive = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideReactionIndicators();
            UIManager.Instance.HideVisualTimer();
        }

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

        Debug.Log($"⚡ EvaluateRound: {alivePlayers.Count} alive players");
        Debug.Log($"⚡ {playerReactionTimes.Count} players tapped during green light");
        Debug.Log($"⚡ {tappedDuringRed.Count} players tapped during red light");

        // Find the slowest tapper (or someone who didn't tap)
        string slowestPlayerId = null;
        long slowestTime = -1;

        // First priority: Anyone who didn't tap at all
        foreach (var player in alivePlayers)
        {
            if (!playerReactionTimes.ContainsKey(player.Id) && !tappedDuringRed.Contains(player.Id))
            {
                slowestPlayerId = player.Id;
                break; // Found non-tapper
            }
        }

        // If everyone tapped, find the slowest
        if (slowestPlayerId == null && playerReactionTimes.Count > 0)
        {
            var slowest = playerReactionTimes.OrderByDescending(kvp => kvp.Value).First();
            slowestPlayerId = slowest.Key;
            slowestTime = slowest.Value;
        }

        // Ensure slowest is marked for elimination (might already be marked from CheckForLastPlayer)
        // Ticks are already showing from OnTapReceived
        foreach (var player in alivePlayers)
        {
            // People who tapped during red are already marked
            if (tappedDuringRed.Contains(player.Id))
            {
                if (!markedForElimination.Contains(player.Id))
                {
                    Debug.Log($"❌ Mask #{player.MaskId} marked for elimination (tapped during red)");
                    markedForElimination.Add(player.Id);
                }
                continue;
            }

            // Mark slowest if not already marked
            if (player.Id == slowestPlayerId && !markedForElimination.Contains(player.Id))
            {
                Debug.Log($"❌ Mask #{player.MaskId} marked for elimination (slowest/no tap)");

                // Show cross if not already showing (CheckForLastPlayer might have shown it)
                if (MaskManager.Instance != null)
                {
                    MaskManager.Instance.ShowCrossOverlay(player.MaskId, 999f);
                }

                markedForElimination.Add(player.Id);
            }
        }

        Debug.Log($"⚡ Reaction Round complete - keeping all masks visible for now");

        // End round after showing results
        Invoke(nameof(EndRound), 5f);
    }

    void EndRound()
    {
        // Eliminate everyone marked with a cross
        int eliminatedCount = 0;

        foreach (string playerId in markedForElimination)
        {
            var player = PlayerManager.Instance.GetPlayer(playerId);
            if (player != null && player.IsAlive)
            {
                Debug.Log($"💀 Eliminating {playerId} (Mask #{player.MaskId})");
                PlayerManager.Instance.EliminatePlayer(playerId, "reaction_round_failed");
                eliminatedCount++;
            }
        }

        Debug.Log($"⚡ Reaction Round: {eliminatedCount} players eliminated");

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.ClearAllOverlays();
        }

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
    }

    void ShowWinner()
    {
        Debug.Log("🏆 Only 1 player remaining - showing winner!");

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.ClearMasks();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
            UIManager.Instance.HideReactionIndicators();
        }

        // The final alive player is the winner!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete(); // Will trigger EndGame which shows the winner
        }
    }
}
